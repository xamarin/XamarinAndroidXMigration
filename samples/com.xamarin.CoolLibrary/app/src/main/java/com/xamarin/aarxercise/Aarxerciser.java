package com.xamarin;

import android.content.Context;
import android.widget.TextView;

public class Aarxerciser {

    public android.support.v4.app.Fragment CreateFragment(Context context)
    {
        SimpleFragment fragment = new SimpleFragment();
        return fragment;
    }

    public SimpleFragment CreateSimpleFragment(Context context)
    {
        SimpleFragment fragment = new SimpleFragment();
        return fragment;
    }

    public void UpdateSimpleFragment(SimpleFragment fragment, String text)
    {
        ((TextView)fragment.getView().findViewById(R.id.simpleFragmentTextView)).setText(text);
    }

    public void UpdateFragment(android.support.v4.app.Fragment fragment, String text)
    {
        ((TextView)fragment.getView().findViewById(R.id.simpleFragmentTextView)).setText(text);
    }
}
